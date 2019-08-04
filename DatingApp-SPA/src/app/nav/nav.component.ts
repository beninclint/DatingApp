import { Component, OnInit } from "@angular/core";
import { AuthService } from "../_services/auth.service";
import { AlertifyService } from '../_services/alertify.service';
import { error } from 'util';

@Component({
  selector: "app-nav",
  templateUrl: "./nav.component.html",
  styleUrls: ["./nav.component.css"]
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(public authService: AuthService, private alertify: AlertifyService) {}

  login() {
    this.authService.login(this.model).subscribe(
      next => {
        this.alertify.success('Logged in successfully');
      }, error => {
this.alertify.error(error)
      }
    );
  }
  ngOnInit() {}

  loggedIn() {
    return this.authService.loggedIn();
  }
  logOut() {
    localStorage.removeItem('token');
    this.alertify.message('logged out');
  }
}