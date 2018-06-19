import { HttpParams, HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';
import { HiveSection } from '../models/hive-section';
import { HiveSectionListItem } from '../models/hive-section-list-item';

@Injectable({
  providedIn: 'root'
})
export class HiveSectionService {
  private url = environment.apiUrl + 'api/sections/';

  constructor(private http: HttpClient) { }

  getHiveSections(): Observable<Array<HiveSectionListItem>> {
    return this.http.get<Array<HiveSectionListItem>>(this.url);
  }

  getHiveSection(hiveSectionId: number): Observable<HiveSection> {
    return this.http.get<HiveSection>(`${this.url}${hiveSectionId}`);
  }

  setHiveSectionStatus(hiveSectionId: number, deletedStatus: boolean): Observable<Object> {
    const params = new HttpParams().set('hiveSectionId', hiveSectionId.toString());
    params.set('deletedStatus', deletedStatus.toString());

    return this.http.put(`${this.url}${hiveSectionId}/status/${deletedStatus}`, {params});
  }

  addHiveSection(createRequest: HiveSection): Observable<HiveSection> {
    return this.http.post<HiveSection>(`${this.url}`, createRequest);
  }

  updateHiveSection(hivesectionId : number, updateRequest: HiveSection): Observable<Object> {
    return this.http.put<Object>(`${this.url}${hivesectionId}`, updateRequest);
  }

  deleteHiveSection(hiveSectionId: number): Observable<Object> {
    return this.http.delete<Object>(`${this.url}${hiveSectionId}`);
  }
}
