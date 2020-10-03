import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})

export class AppComponent implements OnInit {
  
  form = new FormGroup({
    field1: new FormControl('', [Validators.required]),
    field2: new FormControl('Текст из RabbitMQ'),
  });

  @ViewChild('rabbText') rabbText: ElementRef;

  private ws: WebSocket;

ngOnInit(){
  this.ws = new WebSocket("ws://localhost:8000");
  this.ws.onmessage = this.setDataToField;
}

private setDataToField(event){
  console.log(event.data);
  document.getElementById('field2')["value"] = event.data;
}

  get formField1() {
    return this.form.get('field1'); 
  }

  get formField2() {
    return this.form.get('field2');
  }

  send() {
    if (this.form.valid) {
      var login = this.formField1.value;
      this.ws.send(login);
    } else {
      this.form.markAllAsTouched();
    }
  }
}
