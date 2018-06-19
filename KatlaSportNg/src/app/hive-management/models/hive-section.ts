export class HiveSection {
    constructor(
      public id: number,
      public name: string,
      public code: string,
      public HiveSectionCount: number,
      public isDeleted: boolean,
      public lastUpdated: string,
      public storeHiveId: number,
    ) { }
};
