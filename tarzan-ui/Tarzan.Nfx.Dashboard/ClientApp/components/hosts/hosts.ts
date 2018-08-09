import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { Host } from '../../api/models';

@Component
export default class HostsComponent extends Vue {
    loading: boolean = true;
    dataSource: Host[] = [];
    currentPage: number = 1;
    totalItems: number = 0;
    itemFilter: string = "*";
    perPage = 10;

    mounted() {
        fetch('api/hosts/count').then(response => response.text().then(value => this.totalItems = parseInt(value)));

        this.reload(1);
    }

    reload(page: number) {
        this.loading = true;
        let offset = (page - 1) * this.perPage;
        let fetchString = `api/hosts/range/${offset}/count/${this.perPage}`;
        console.log(fetchString);
        fetch(fetchString)
            .then(response => response.json() as Promise<Host[]>)
            .then(data => {
                this.dataSource = data;
                this.loading = false;
            });
    }
}


