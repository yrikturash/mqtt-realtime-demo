import React, { useState, useEffect } from "react";
import { MapContainer, TileLayer } from 'react-leaflet';
import data from "../assets/data.json";

import { useLocation, useHistory } from "react-router-dom";
import MqttConnector from "./MqttConnector";
import RealtimeComponent from "./RealTime"

import "leaflet/dist/leaflet.css";

const lvivCenterPosition = {lat: 49.83457901256914,  lng: 24.028424156611234};
const MQTT_BROKER = "ws://localhost:15675/ws";
const selectedRouteId = "1630";

const MapView = (props) => {
  const [state, setState] = useState({
    currentLocation: lvivCenterPosition,
    zoom: 13,
    data,
  });

  const location = useLocation();
  const history = useHistory();

  useEffect(() => {
    if (location.state.latitude && location.state.longitude) {
      const currentLocation = {
        lat: location.state.latitude,
        lng: location.state.longitude,
      };
      console.log(state);
      setState({
        ...state,
        data: {
          venues: state.data.venues.concat({
            name: "new",
            geometry: [currentLocation.lat, currentLocation.lng],
          }),
        },
        currentLocation,
      });
      history.replace({
        pathname: "/map",
        state: {},
      });
    }

  }, [location]);


  
  return (
    <MapContainer center={state.currentLocation} zoom={state.zoom}>
      <TileLayer
        url="https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}"
        attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        id='mapbox/streets-v11'
        accessToken='pk.eyJ1IjoiZGFudGVkcmVhbWVyIiwiYSI6ImNrcXIyNGpsajBrbTgybnEzazc5Y3RnNWgifQ.4YQxTUFcsxT3GjlX8-nuzg'
      />

        <MqttConnector brokerUrl={MQTT_BROKER}>
          <RealtimeComponent routeId={selectedRouteId} />
        </MqttConnector>
      {/* <Markers venues={state.data.venues} /> */}
    </MapContainer>
  );
};

export default MapView;
