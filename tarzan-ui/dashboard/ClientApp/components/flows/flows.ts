import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { FlowRecord } from '../../api/flowRecord';

@Component
export default class FetchDataComponent extends Vue {
    dataSource: FlowRecord[] = [];
    currentPage: number = 1;
    totalFlows: number = 0;
    perPage = 10;
    fields = ['id', 'protocol', 'sourceAddress', 'sourcePort'];
    mounted() {
        fetch('api/flows/count').then(response => response.text().then(value => this.totalFlows = parseInt(value)));

        this.reload(1);
    }

    reload(page:number) {
        let offset = (page - 1) * this.perPage;
        let fetchString = `api/flows/range/${offset}/count/${this.perPage}`;
        console.log(fetchString);
        fetch(fetchString)
        .then(response => response.json() as Promise<FlowRecord[]>)
        .then(data => {
            this.dataSource = data;
        });        
    }
}
