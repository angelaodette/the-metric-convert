import { Routes } from '@angular/router';
import { ConverterComponent } from './converter.component';
import { LearnComponent } from './learn.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: ConverterComponent
  },
  {
    path: 'learn',
    component: LearnComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
