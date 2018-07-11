import Vue from 'vue';
import { Component } from 'vue-property-decorator';

/**
* Represents a single flow record.
*/
export class FlowRecord {
    /**
    * A unique identifier of the flow record.
    */
    'id': number;
    /**
    * Type of transport (or internet) protocol of the flow.
    */
    'protocol': string;
    /**
    * The network source address of the flow.
    */
    'sourceAddress': string;
    /**
    * Source port (if any) of the flow.
    */
    'sourcePort': number;
    /**
    * The network destination address of the flow.
    */
    'destinationAddress': string;
    /**
    * The destination port of the flow.
    */
    'destinationPort': number;
    /**
    * Unix time stamp of the start of flow.
    */
    'firstSeen': number;
    /**
    * The unix time stamp of the end of flow.
    */
    'lastSeen': number;
    /**
    * Number of packets carried by the flow.
    */
    'packets': number;
    /**
    * Total number of octets carried by the flow.
    */
    'octets': number;

    static discriminator = undefined;

    static attributeTypeMap: Array<{ name: string, baseName: string, type: string }> = [
        {
            "name": "id",
            "baseName": "id",
            "type": "number"
        },
        {
            "name": "protocol",
            "baseName": "protocol",
            "type": "string"
        },
        {
            "name": "sourceAddress",
            "baseName": "sourceAddress",
            "type": "string"
        },
        {
            "name": "sourcePort",
            "baseName": "sourcePort",
            "type": "number"
        },
        {
            "name": "destinationAddress",
            "baseName": "destinationAddress",
            "type": "string"
        },
        {
            "name": "destinationPort",
            "baseName": "destinationPort",
            "type": "number"
        },
        {
            "name": "firstSeen",
            "baseName": "firstSeen",
            "type": "number"
        },
        {
            "name": "lastSeen",
            "baseName": "lastSeen",
            "type": "number"
        },
        {
            "name": "packets",
            "baseName": "packets",
            "type": "number"
        },
        {
            "name": "octets",
            "baseName": "octets",
            "type": "number"
        }];

    static getAttributeTypeMap() {
        return FlowRecord.attributeTypeMap;
    }
}

@Component
export default class FetchDataComponent extends Vue {
    forecasts: FlowRecord[] = [];

    mounted() {
        fetch('api/flows')
            .then(response => response.json() as Promise<FlowRecord[]>)
            .then(data => {
                this.forecasts = data;
            });
    }
}
