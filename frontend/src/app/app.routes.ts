import { Routes } from '@angular/router';
import { ConverterComponent } from './converter.component';
import { LearnComponent } from './learn.component';
import { HomeComponent } from './home.component';
import { LoginComponent } from './login.component';
import { RegisterComponent } from './register.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: HomeComponent
  },
  {
    path: 'converter',
    component: ConverterComponent
  },
  {
    path: 'learn',
    component: LearnComponent
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
