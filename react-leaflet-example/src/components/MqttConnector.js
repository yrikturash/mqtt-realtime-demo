/* eslint-disable no-console */
import React, { useEffect, useState, useCallback, useRef } from 'react';
import { connect } from 'mqtt';
import MqttContext from '../context/MqttContext';
import proto from '../utils/protobuf';

export default function MqttConnector({ children, brokerUrl }) {
  // ref is needed to track if component is mounted
  const mountedRef = useRef(true);
  const [client, setClient] = useState(null);
  const [message, setMessage] = useState(null);

  const mqttConnect = useCallback(async () => {
    const options = {
      keepalive: 55,
      connectTimeout: 30000,
      clean: true,
      protocolId: 'MQTT',
      protocolVersion: 4
    };
    try {
      const mqtt = connect(brokerUrl, options);
      mqtt.on('connect', () => {
        if (mountedRef.current) {
          setClient(mqtt);
        }
      });
      mqtt.on('close', () => {
        console.log('close');
      });
      mqtt.on('reconnect', () => {
        console.log('reconnect');
      });
      mqtt.on('disconnect', () => {
        console.log('disconnect');
      });
      mqtt.on('error', err => {
        console.log(`Connection error: ${err}`);
      });
      mqtt.on('offline', () => {
        console.log('offline');
      });
      mqtt.on('end', () => {
        console.log('end');
      });
    } catch (error) {
      console.log(`Connection error: ${error}`);
    }
  }, [brokerUrl]);

  useEffect(() => {
    if (client) {
      client.on('message', (topic, msg) => {
        proto(msg).then(res => {
          const payload = {
            topic,
            message: res
          };
          setMessage(payload);
        });
      });
    } else {
      mqttConnect();
    }

    return () => {
      mountedRef.current = false;

      client?.end(true);
    };
  }, [client, mqttConnect]);

  return (
    <MqttContext.Provider value={{ client, message }}>
      {children}
    </MqttContext.Provider>
  );
}
