import { useCallback, useEffect, useMemo } from 'react';
import ReactDOMServer from 'react-dom/server';
import L from 'leaflet';
import 'leaflet-rotatedmarker';
import { useMap, useMapEvent } from 'react-leaflet';
import 'leaflet.marker.slideto';
import TransportIcon from './TransportIcon';

const TransportOnMap = ({ message, routeTextColor }) => {
  const map = useMap();
  const mapLayer = useMemo(() => new L.FeatureGroup(), []);
  const markerList = useMemo(() => ({}), []);
  const MAX_MARKER_VISIBLE_ZOOM = 12;

  const iconHtml = useMemo(
    () =>
      ReactDOMServer.renderToStaticMarkup(
        <TransportIcon fillColor={routeTextColor} />
      ),
    [routeTextColor]
  );

  const icon = useMemo(
    () =>
      L.divIcon({
        html: iconHtml,
        iconAnchor: [20, 20],
        className: ''
      }),
    [iconHtml]
  );

  const setMarkers = useCallback(
    ({ licensePlateNumber, vehiclePosition }) => {
      console.log("setMarkers", licensePlateNumber, vehiclePosition);
      const { latitude, longitude, bearing } = vehiclePosition;
      if (markerList[licensePlateNumber]) {
        markerList[licensePlateNumber].slideTo([latitude, longitude], {
          duration: 600,
          keepAtCenter: false
        });

        markerList[licensePlateNumber].setRotationAngle(bearing);
      } else {
        markerList[licensePlateNumber] = L.marker(
          [latitude, longitude],
          // @ts-ignore
          { icon, rotationAngle: bearing }
        );

        mapLayer.addLayer(markerList[licensePlateNumber]).addTo(map);
      }
    },
    [icon, map, mapLayer, markerList]
  );

  const toggleMarkersShown = useCallback(
    () =>
      map.getZoom() > MAX_MARKER_VISIBLE_ZOOM
        ? map.addLayer(mapLayer)
        : map.removeLayer(mapLayer),
    [map, mapLayer]
  );

  useMapEvent('zoomend', toggleMarkersShown);

  useEffect(() => {
    console.log("use effect:", map, message)
    if (map && message) {
      setMarkers(message.message);
    }
  }, [map, message, setMarkers]);

  useEffect(() => {
    toggleMarkersShown();
  }, [toggleMarkersShown]);

  useEffect(() => {
    return () => {
      map.removeLayer(mapLayer);
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return null;
};

export default TransportOnMap;
