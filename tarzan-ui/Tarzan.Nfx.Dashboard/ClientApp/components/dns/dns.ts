import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { DnsInfo } from '../../api/models';
@Component
export default class DnsComponent extends Vue {
    loading: boolean = true;
    dataSource: DnsInfo[] = [];
    currentPage: number = 1;
    totalItems: number = 0;
    serviceFilter: string = "*";
    perPage = 10;
    mounted() {
        fetch('api/dns/count').then(response => response.text().then(value => this.totalItems = parseInt(value)));
        this.reload(1);
    }

    reload(page: number) {
        this.loading = true;
        let offset = (page - 1) * this.perPage;
        let fetchString = `api/dns/range/${offset}/count/${this.perPage}`;
        console.log(fetchString);
        fetch(fetchString)
            .then(response => response.json() as Promise<DnsInfo[]>)
            .then(data => {
                this.dataSource = data;
                this.loading = false;
            });
    }
}


