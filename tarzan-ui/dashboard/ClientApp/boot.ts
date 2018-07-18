import Vue from 'vue';
import VueRouter from 'vue-router';


import ElementUI from 'element-ui';
import 'element-ui/lib/theme-chalk/index.css';

import './css/site.css';

Vue.use(VueRouter);
Vue.use(ElementUI);

const routes = [
    { path: '/', component: require('./components/about/about.vue.html') },
    { path: '/captures', component: require('./components/captures/captures.vue.html') },
    { path: '/flows/flowtable', component: require('./components/flowtable/flowtable.vue.html') },
    { name: 'flowinfo', path: '/flows/details/:id', component: require('./components/flow/flow.vue.html') },
    { path: '/flows/details', component: require('./components/flow/flow.vue.html') },
    { path: '/flows/sessions', component: require('./components/sessions/sessions.vue.html') },

    { path: '/explore/hosts', component: require('./components/hosts/hosts.vue.html') },
    { path: '/explore/services', component: require('./components/services/services.vue.html') },
    { path: '/explore/dns', component: require('./components/dns/dns.vue.html') },
    { path: '/explore/http', component: require('./components/http/http.vue.html') },
    { path: '/explore/tls', component: require('./components/tls/tls.vue.html') },

    { path: '/results/cleartext', component: require('./components/cleartext/cleartext.vue.html') },
    { path: '/results/credentials', component: require('./components/credentials/credentials.vue.html') },
    { path: '/results/keywordsearch', component: require('./components/keywordsearch/keywordsearch.vue.html') },

    { path: '/files', component: require('./components/files/files.vue.html') },
    { path: '/timeline', component: require('./components/timeline/timeline.vue.html') },
    { path: '/tags', component: require('./components/tags/tags.vue.html') },
    { path: '/logs', component: require('./components/logs/logs.vue.html') },
    { path: '/settings', component: require('./components/settings/settings.vue.html') },
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(require('./components/app/app.vue.html'))
});

