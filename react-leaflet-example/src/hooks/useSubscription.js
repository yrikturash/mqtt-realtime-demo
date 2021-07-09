import { useContext, useEffect } from 'react';
import MqttContext from '../context/MqttContext';

export default function useSubscription(topic) {
  const { client, message } = useContext(MqttContext);

  useEffect(() => {
    if (client?.connected) {
      client.subscribe(topic);
    }

    return () => {
      client?.unsubscribe(topic);
    };
  }, [client, topic]);

  return {
    client,
    message
  };
}
