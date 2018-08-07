import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { FlowRecord } from '../../api/flowRecord';

@Component
export default class FlowTableComponent extends Vue {
    dataSource: FlowRecord[] = [];
    currentPage: number = 1;
    totalFlows: number = 0;
    flowFilter: string = "*";
    perPage = 10;
    loading = true;
    mounted() {
        fetch('api/flows/count').then(response => response.text().then(value => this.totalFlows = parseInt(value)));

        this.reload(1);
    }

    reload(page: number) {
        this.loading = true;
        let offset = (page - 1) * this.perPage;
        let fetchString = `api/flows/range/${offset}/count/${this.perPage}`;
        console.log(fetchString);
        fetch(fetchString)
        .then(response => response.json() as Promise<FlowRecord[]>)
        .then(data => {
            this.dataSource = data;
            this.loading = false;
        });        
    }

    selectFlow(row:FlowRecord) {
        console.log('Double Click, row.flowid=' + row.flowId);
        this.$router.push({ name: 'flowinfo', params: { id: row.flowId } })
    }
}
