import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { FlowRecord } from '../../api/flowRecord';

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
