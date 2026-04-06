import { platformBrowser } from '@angular/platform-browser';
import { AppModule } from './app/app-module';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';

ModuleRegistry.registerModules([AllCommunityModule]);

platformBrowser().bootstrapModule(AppModule, {
  
})
  .catch(err => console.error(err));
