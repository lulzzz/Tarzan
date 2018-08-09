import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { Service } from '../../api/models';

@Component
export default class ServicesComponent extends Vue {
    loading: boolean = false;
    dataSource: Service[] = [];
    currentPage: number = 1;
    totalItems: number = 0;
    serviceFilter: string = "*";
    perPage = 10;
    mounted() {
        fetch('api/services/count').then(response => response.text().then(value => this.totalItems = parseInt(value)));
        this.reload(1);
    }

    reload(page: number) {
        this.loading = true;
        let offset = (page - 1) * this.perPage;
        let fetchString = `api/services/range/${offset}/count/${this.perPage}`;
        console.log(fetchString);
        fetch(fetchString)
            .then(response => response.json() as Promise<Service[]>)
            .then(data => {
                this.dataSource = data;
                this.loading = false;
            });
    }
}


