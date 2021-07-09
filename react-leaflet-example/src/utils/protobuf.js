import protobuf from 'protobufjs';
import protofile from './../protos/gps-event.proto'

export default function proto(buffer) {
  return protobuf
    .load(protofile)
    .then(root => {
    console.log("try to decode:", buffer)
      var GpsEvent = root.lookupType(
        'ss.protos.v1.GpsEvent'
      );
      var message = GpsEvent.decode(buffer);
      console.log("decoded:", message)
      
      return message;
    })
    .catch(err => {
      console.log(err);
    });
}
