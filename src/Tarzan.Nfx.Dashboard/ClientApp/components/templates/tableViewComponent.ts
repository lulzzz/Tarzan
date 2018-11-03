import Vue from 'vue';
import { Component } from 'vue-property-decorator';

export class TableViewComponent<ViewType> extends Vue {
    loading: boolean = true;
    dataSource: ViewType[] = [];
    currentPage: number = 1;
    totalItems: number = 0;
    filter: string = "*";
    perPage = 10;
    fetchPrefix = "";
    queryParams = "";
    constructor(fetchPrefix: string) {
        super();
        this.fetchPrefix = fetchPrefix;
    }

    protected fetch() {
        fetch(this.fetchPrefix + 'count' + this.queryParams).then(response => response.text().then(value => this.totalItems = parseInt(value)));
        this.reload(1);
    }

    protected reload(page: number) {
        this.loading = true;
        let offset = (page - 1) * this.perPage;
        let fetchString = this.fetchPrefix + `range/${offset}/count/${this.perPage}` + this.queryParams;
        console.log(fetchString);
        fetch(fetchString)
            .then(response => response.json() as Promise<ViewType[]>)
            .then(data => {
                this.dataSource = data;
                this.loading = false;
            });
    }
}