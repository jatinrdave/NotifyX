import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FlowCanvasComponent } from './components/canvas/flow-canvas.component';
import { NodePaletteComponent } from './components/palette/node-palette.component';
import { RunInspectorComponent } from './components/run/run-inspector.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FlowCanvasComponent,
    NodePaletteComponent,
    RunInspectorComponent
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
