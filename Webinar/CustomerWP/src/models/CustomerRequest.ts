export type NdaDecision = 'Yes' | 'No' | undefined;

export interface ICustomerRequest {
  title: string;
  sectorId: number;
  sectorTitle: string;
  sectorRequiresNda: boolean;
  ndaDecision: NdaDecision;
}
