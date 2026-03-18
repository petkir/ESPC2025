import * as React from 'react';
import * as ReactDom from 'react-dom';
import { Version } from '@microsoft/sp-core-library';
import {
  type IPropertyPaneConfiguration,
  PropertyPaneTextField,
  PropertyPaneToggle
} from '@microsoft/sp-property-pane';
import { BaseClientSideWebPart } from '@microsoft/sp-webpart-base';
import { IReadonlyTheme } from '@microsoft/sp-component-base';

import * as strings from 'CustomerFormWebPartStrings';
import CustomerForm from './components/CustomerForm';
import { ICustomerFormProps } from './components/ICustomerFormProps';

import { MockCustomerFormDataService } from '../../services/MockCustomerFormDataService';
import { SpCustomerFormDataService } from '../../services/SpCustomerFormDataService';
import { NullTelemetryService } from '../../services/NullTelemetryService';
import { ApplicationInsightsTelemetryService } from '../../services/ApplicationInsightsTelemetryService';
import type { ICustomerFormDataService } from '../../services/ICustomerFormDataService';
import type { ITelemetryService } from '../../services/ITelemetryService';

export interface ICustomerFormWebPartProps {
  description: string;
  sectorsListTitle: string;
  requestsListTitle: string;
  useMockData: boolean;
  appInsightsConnectionString: string;
}

export default class CustomerFormWebPart extends BaseClientSideWebPart<ICustomerFormWebPartProps> {

  private _isDarkTheme: boolean = false;
  private _environmentMessage: string = '';

  private _getEnvString(value: string): string {
    return (value || '').trim();
  }

  private _getEnvBoolean(value: string): boolean | undefined {
    const normalized = (value || '').trim().toLowerCase();
    if (!normalized) {
      return undefined;
    }

    if (['1', 'true', 'yes', 'y', 'on'].indexOf(normalized) !== -1) {
      return true;
    }

    if (['0', 'false', 'no', 'n', 'off'].indexOf(normalized) !== -1) {
      return false;
    }

    return undefined;
  }

  private _applyBuildTimeDefaults(): void {
    const envSectorsListTitle = this._getEnvString(CUSTOMERWP_SECTORS_LIST_TITLE);
    const envRequestsListTitle = this._getEnvString(CUSTOMERWP_REQUESTS_LIST_TITLE);
    const envAppInsightsConnectionString = this._getEnvString(CUSTOMERWP_APPINSIGHTS_CONNECTION_STRING);
    const envUseMockData = this._getEnvBoolean(CUSTOMERWP_USE_MOCK_DATA);

    if (envSectorsListTitle) {
      this.properties.sectorsListTitle = envSectorsListTitle;
    }

    if (envRequestsListTitle) {
      this.properties.requestsListTitle = envRequestsListTitle;
    }

    if (envAppInsightsConnectionString) {
      this.properties.appInsightsConnectionString = envAppInsightsConnectionString;
    }

    if (envUseMockData !== undefined) {
      this.properties.useMockData = envUseMockData;
    }
  }

  private _getDataService(): ICustomerFormDataService {
    const useMock = this.properties.useMockData || this.context.isServedFromLocalhost;

    if (useMock) {
      return new MockCustomerFormDataService();
    }

    return new SpCustomerFormDataService(this.context, {
      sectorsListTitle: this.properties.sectorsListTitle || this._getEnvString(CUSTOMERWP_SECTORS_LIST_TITLE) || 'Sector',
      requestsListTitle: this.properties.requestsListTitle || this._getEnvString(CUSTOMERWP_REQUESTS_LIST_TITLE) || 'CustomerRequest'
    });
  }

  private _getTelemetryService(): ITelemetryService {
    const cs = (this.properties.appInsightsConnectionString || this._getEnvString(CUSTOMERWP_APPINSIGHTS_CONNECTION_STRING) || '').trim();
    if (!cs) {
      return new NullTelemetryService();
    }

    return new ApplicationInsightsTelemetryService({ connectionString: cs });
  }

  public render(): void {
    const dataService = this._getDataService();
    const telemetryService = this._getTelemetryService();

    const element: React.ReactElement<ICustomerFormProps> = React.createElement(
      CustomerForm,
      {
        hasTeamsContext: !!this.context.sdks.microsoftTeams,
        dataService,
        telemetryService
      }
    );

    ReactDom.render(element, this.domElement);
  }

  protected onInit(): Promise<void> {
    this._applyBuildTimeDefaults();

    return this._getEnvironmentMessage().then(message => {
      this._environmentMessage = message;
    });
  }



  private _getEnvironmentMessage(): Promise<string> {
    if (!!this.context.sdks.microsoftTeams) { // running in Teams, office.com or Outlook
      return this.context.sdks.microsoftTeams.teamsJs.app.getContext()
        .then(context => {
          let environmentMessage: string = '';
          switch (context.app.host.name) {
            case 'Office': // running in Office
              environmentMessage = this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentOffice : strings.AppOfficeEnvironment;
              break;
            case 'Outlook': // running in Outlook
              environmentMessage = this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentOutlook : strings.AppOutlookEnvironment;
              break;
            case 'Teams': // running in Teams
            case 'TeamsModern':
              environmentMessage = this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentTeams : strings.AppTeamsTabEnvironment;
              break;
            default:
              environmentMessage = strings.UnknownEnvironment;
          }

          return environmentMessage;
        });
    }

    return Promise.resolve(this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentSharePoint : strings.AppSharePointEnvironment);
  }

  protected onThemeChanged(currentTheme: IReadonlyTheme | undefined): void {
    if (!currentTheme) {
      return;
    }

    this._isDarkTheme = !!currentTheme.isInverted;
    const {
      semanticColors
    } = currentTheme;

    if (semanticColors) {
      this.domElement.style.setProperty('--bodyText', semanticColors.bodyText || null);
      this.domElement.style.setProperty('--link', semanticColors.link || null);
      this.domElement.style.setProperty('--linkHovered', semanticColors.linkHovered || null);
    }

  }

  protected onDispose(): void {
    ReactDom.unmountComponentAtNode(this.domElement);
  }

  protected get dataVersion(): Version {
    return Version.parse('1.0');
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: {
            description: strings.PropertyPaneDescription
          },
          groups: [
            {
              groupName: strings.BasicGroupName,
              groupFields: [
                PropertyPaneTextField('description', {
                  label: strings.DescriptionFieldLabel
                }),
                PropertyPaneTextField('sectorsListTitle', {
                  label: strings.SectorsListTitleLabel
                }),
                PropertyPaneTextField('requestsListTitle', {
                  label: strings.RequestsListTitleLabel
                }),
                PropertyPaneToggle('useMockData', {
                  label: strings.UseMockDataLabel
                }),
                PropertyPaneTextField('appInsightsConnectionString', {
                  label: strings.AppInsightsConnectionStringLabel
                })
              ]
            }
          ]
        }
      ]
    };
  }
}
