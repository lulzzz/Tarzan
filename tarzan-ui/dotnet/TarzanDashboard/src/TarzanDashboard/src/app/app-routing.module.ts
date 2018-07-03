import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { FlowsComponent } from './components/flows/flows.component';
import { CapturesComponent } from './components/captures/captures.component';
import { ExploreComponent } from './components/explore/explore.component';
import { ResultsComponent } from './components/results/results.component';


const routes: Routes = [
  { path: 'home', component: HomeComponent },
  { path: 'flows', component: FlowsComponent },
  { path: 'captures', component: CapturesComponent},
  { path: 'explore', component: ExploreComponent},
  { path: 'results', component: ResultsComponent}
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forRoot(routes)
  ],
  exports: [ RouterModule ],
  declarations: []
})
export class AppRoutingModule { }
