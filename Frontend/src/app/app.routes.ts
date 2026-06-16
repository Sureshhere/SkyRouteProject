import { Routes } from '@angular/router';
import { RegisterComponent } from './auth/register.component';
import { LoginComponent } from './auth/login.component';
import { FlightSearchComponent } from './flights/search.component';
import { FlightResultsComponent } from './flights/results.component';
import { BookingComponent } from './booking/booking.component';
import { ConfirmationComponent } from './booking/confirmation.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/flights', pathMatch: 'full' },
  { path: 'register', component: RegisterComponent },
  { path: 'login', component: LoginComponent },
  { path: 'flights', component: FlightSearchComponent },
  { path: 'results', component: FlightResultsComponent },
  { path: 'booking', component: BookingComponent, canActivate: [authGuard] },
  { path: 'confirmation', component: ConfirmationComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/flights' }
];
