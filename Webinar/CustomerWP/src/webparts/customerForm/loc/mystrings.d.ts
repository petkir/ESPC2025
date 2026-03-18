declare interface ICustomerFormWebPartStrings {
  PropertyPaneDescription: string;
  BasicGroupName: string;
  DescriptionFieldLabel: string;
  SectorsListTitleLabel: string;
  RequestsListTitleLabel: string;
  UseMockDataLabel: string;
  AppInsightsConnectionStringLabel: string;
  AppLocalEnvironmentSharePoint: string;
  AppLocalEnvironmentTeams: string;
  AppLocalEnvironmentOffice: string;
  AppLocalEnvironmentOutlook: string;
  AppSharePointEnvironment: string;
  AppTeamsTabEnvironment: string;
  AppOfficeEnvironment: string;
  AppOutlookEnvironment: string;
  UnknownEnvironment: string;
}

declare module 'CustomerFormWebPartStrings' {
  const strings: ICustomerFormWebPartStrings;
  export = strings;
}
