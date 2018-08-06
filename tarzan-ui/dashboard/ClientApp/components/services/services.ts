import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component
export default class ServicesComponent extends Vue {
    loading: boolean = false;
    dataSource = [{
        protocol: 'HTTP',
        flows: 13256,
        packets: 1242323,
        octets: 242353456,
        minPackets: 2, 
        minOctets: 132,
        maxPackets: 3424,
        maxOctets: 122456,
        avgPackets: 24,
        avgOctets: 35346,
        minDuration: 400,
        maxDuration: 123425,
        avgDuration: 3453
    },
    {
        protocol: 'DNS',
        flows: 13256,
        packets: 1242323,
        octets: 242353456,
        minPackets: 2,
        minOctets: 132,
        maxPackets: 3424,
        maxOctets: 122456,
        avgPackets: 24,
        avgOctets: 35346,
        minDuration: 400,
        maxDuration: 123425,
        avgDuration: 3453
    }
    ];
}


