import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {}; // stores username and password

  constructor(private authService: AuthService) { }

  ngOnInit(): void {
  }

  login(): void {
    console.log(this.model);
    this.authService.login(this.model).subscribe(
      next => {
        console.log('logged in successfully');
      }, error => {
        console.log('login failed');
      }
    );
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !!token;
  }

  logout() {
    localStorage.removeItem('token');
    console.log('logged out');
  }

}
