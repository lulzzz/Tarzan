import './css/site.css';
import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
import BootstrapVue from 'bootstrap-vue';
Vue.use(VueRouter);
Vue.use(BootstrapVue);

const routes = [
    { path: '/', component: require('./components/about/about.vue.html') },
    { path: '/captures', component: require('./components/captures/captures.vue.html') },
    { path: '/flows', component: require('./components/flows/flows.vue.html') }
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(require('./components/app/app.vue.html'))
});
