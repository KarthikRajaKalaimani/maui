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
			handler.PlatformView.ClearMapElements();
			handler.PlatformView.AddElements((IList)map.Elements);
		}

		void UpdateMapElements(NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add when args.NewItems != null:
					QueueMapElementAdds(args.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove when args.OldItems != null:
					QueueMapElementRemoves(args.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace when args.OldItems != null && args.NewItems != null:
					QueueMapElementRemoves(args.OldItems);
					QueueMapElementAdds(args.NewItems);
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
			DispatchQueue.MainQueue.DispatchAfter(
				new DispatchTime(DispatchTime.Now, MapElementBatchInterval),
				ProcessPendingMapElementUpdates);
		}

		void QueueMapElementAdds(IList elements)
		{
			foreach (var item in elements)
			{
				if (item is not IMapElement element)
					continue;

				if (!_pendingMapElementRemoves.Remove(element))
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

			if (_mapElementsRequireFullSync)
			{
				PlatformView.ClearMapElements();
				PlatformView.AddElements((IList)map.Elements);
			}
			else
			{
				if (_pendingMapElementRemoves.Count > 0)
					PlatformView.RemoveElements(_pendingMapElementRemoves.ToArray());

				if (_pendingMapElementAdds.Count > 0)
					PlatformView.AddElements(_pendingMapElementAdds.ToArray());
			}

			ClearPendingMapElementUpdates();
		}

		void ClearPendingMapElementUpdates()
		{
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
			PlatformView.RemoveElements(new[] { element });
			PlatformView.AddElements(new[] { element });
		}

		void MoveToRegion(MapSpan mapSpan, bool animated = true)
		{
			var center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
			PlatformView.SetRegion(mapRegion, animated);
		}
	}
}
