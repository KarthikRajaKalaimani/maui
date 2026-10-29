using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CoreFoundation;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Platform;

namespace Microsoft.Maui.Maps.Handlers
{

	public partial class MapHandler : ViewHandler<IMap, MauiMKMapView>
	{
		static readonly TimeSpan MapElementBatchInterval = TimeSpan.FromMilliseconds(16);

		CLLocationManager? _locationManager;
		readonly HashSet<IMapElement> _pendingMapElementAdds = new(ReferenceEqualityComparer.Instance);
		readonly HashSet<IMapElement> _pendingMapElementRemoves = new(ReferenceEqualityComparer.Instance);
		bool _mapElementUpdateScheduled;
		bool _mapElementsRequireFullSync;
		bool _isConnected;
		int _mapElementUpdateVersion;

		protected override MauiMKMapView CreatePlatformView()
		{
			return MapPool.Get(this) ?? new MauiMKMapView(this);
		}

		protected override void ConnectHandler(MauiMKMapView platformView)
		{
			base.ConnectHandler(platformView);
			_isConnected = true;
			_locationManager = new CLLocationManager();
		}

		protected override void DisconnectHandler(MauiMKMapView platformView)
		{
			_isConnected = false;
			ClearPendingMapElementUpdates();
			base.DisconnectHandler(platformView);

			// This handler is done with the MKMapView; we can put it in the pool
			// for other rendererers to use in the future
			platformView.PrepareForReuse();
			MapPool.Add(platformView);
		}

		public static void MapMapType(IMapHandler handler, IMap map)
		{
			switch (map.MapType)
			{
				case MapType.Street:
					handler.PlatformView.MapType = MKMapType.Standard;
					break;
				case MapType.Satellite:
					handler.PlatformView.MapType = MKMapType.Satellite;
					break;
				case MapType.Hybrid:
					handler.PlatformView.MapType = MKMapType.Hybrid;
					break;
			}
		}

		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{
#if !MACCATALYST
			if (map.IsShowingUser)
			{
				MapHandler? mapHandler = handler as MapHandler;
				mapHandler?._locationManager?.RequestWhenInUseAuthorization();
			}
#endif
			handler.PlatformView.ShowsUserLocation = map.IsShowingUser;
		}

		public static void MapIsScrollEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ScrollEnabled = map.IsScrollEnabled;
		}

		public static void MapIsTrafficEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ShowsTraffic = map.IsTrafficEnabled;
		}

		public static void MapIsZoomEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ZoomEnabled = map.IsZoomEnabled;
		}

		public static void MapPins(IMapHandler handler, IMap map)
		{
			handler.PlatformView.AddPins((IList)map.Pins);
		}

		public static void MapElements(IMapHandler handler, IMap map)
		{
			(handler as MapHandler)?.ClearPendingMapElementUpdates();
			handler.PlatformView.ClearMapElements();
			handler.PlatformView.AddElements((IList)map.Elements);
		}

		void UpdateMapElements(NotifyCollectionChangedEventArgs args)
		{
			if (!_isConnected || ((IElementHandler)this).VirtualView is not IMap)
				return;

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add when args.NewItems != null:
					QueueMapElementAdds(args.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove when args.OldItems != null:
					QueueMapElementRemoves(args.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace when args.OldItems != null && args.NewItems != null:
					_mapElementsRequireFullSync = true;
					break;
				default:
					_pendingMapElementAdds.Clear();
					_pendingMapElementRemoves.Clear();
					_mapElementsRequireFullSync = true;
					break;
			}

			if (_mapElementUpdateScheduled)
				return;

			_mapElementUpdateScheduled = true;
			var version = _mapElementUpdateVersion;
			DispatchQueue.MainQueue.DispatchAfter(
				new DispatchTime(DispatchTime.Now, MapElementBatchInterval),
				() =>
				{
					if (version == _mapElementUpdateVersion)
						ProcessPendingMapElementUpdates();
				});
		}

		void QueueMapElementAdds(IList elements)
		{
			foreach (var item in elements)
			{
				if (item is not IMapElement element)
					continue;

				if (_pendingMapElementRemoves.Remove(element))
					_mapElementsRequireFullSync = true;
				else
					_pendingMapElementAdds.Add(element);
			}
		}

		void QueueMapElementRemoves(IList elements)
		{
			foreach (var item in elements)
			{
				if (item is not IMapElement element)
					continue;

				if (!_pendingMapElementAdds.Remove(element))
					_pendingMapElementRemoves.Add(element);
			}
		}

		void ProcessPendingMapElementUpdates()
		{
			_mapElementUpdateScheduled = false;

			if (!_isConnected || ((IElementHandler)this).VirtualView is not IMap map)
			{
				ClearPendingMapElementUpdates();
				return;
			}

			var elements = map.Elements;
			var additions = new List<IMapElement>(_pendingMapElementAdds.Count);
			if (!_mapElementsRequireFullSync && _pendingMapElementAdds.Count > 0)
			{
				foreach (var element in elements)
				{
					if (_pendingMapElementAdds.Contains(element))
						additions.Add(element);
					else if (additions.Count > 0)
					{
						// Appending native overlays cannot represent a managed insertion.
						_mapElementsRequireFullSync = true;
						break;
					}
				}
			}

			if (_mapElementsRequireFullSync)
			{
				PlatformView.ClearMapElements();
				PlatformView.AddElements((IList)elements);
			}
			else
			{
				if (_pendingMapElementRemoves.Count > 0)
					PlatformView.RemoveElements(_pendingMapElementRemoves.ToArray());

				if (additions.Count > 0)
					PlatformView.AddElements(additions);
			}

			ClearPendingMapElementUpdates();
		}

		void ClearPendingMapElementUpdates()
		{
			_mapElementUpdateVersion++;
			_pendingMapElementAdds.Clear();
			_pendingMapElementRemoves.Clear();
			_mapElementUpdateScheduled = false;
			_mapElementsRequireFullSync = false;
		}

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			MapSpan? newRegion = arg as MapSpan;
			if (newRegion != null)
				(handler as MapHandler)?.MoveToRegion(newRegion, true);
		}

		public void UpdateMapElement(IMapElement element)
		{
			if (!_isConnected || _mapElementsRequireFullSync ||
				_pendingMapElementAdds.Contains(element) || _pendingMapElementRemoves.Contains(element) ||
				element.MapElementId is not IMKOverlay overlay)
				return;

			var index = Array.FindIndex(PlatformView.Overlays, candidate => candidate.Handle == overlay.Handle);
			if (index < 0)
				return;

			PlatformView.RemoveElements(new[] { element });
			PlatformView.AddElements(new[] { element }, index);
		}

		void MoveToRegion(MapSpan mapSpan, bool animated = true)
		{
			var center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
			PlatformView.SetRegion(mapRegion, animated);
		}
	}
}
