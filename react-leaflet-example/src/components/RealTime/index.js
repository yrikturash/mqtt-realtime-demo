import React  from 'react';
import useSubscription from '../../hooks/useSubscription';
import TransportOnMap from './TransportOnMap';

const RealtimeComponent = ({
  routeId,
  routeTextColor = '#2196f3'
}) => {
  const topic = `events/gps/routes/${routeId}`;
  const { message } = useSubscription(topic);

  console.log(message);
  return (
    <TransportOnMap
      message={message}
      routeTextColor={routeTextColor}
    />
  );
};

export default RealtimeComponent;
