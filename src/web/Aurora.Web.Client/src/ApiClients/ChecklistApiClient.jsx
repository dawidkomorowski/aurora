import { ApiClient } from "./ApiClient";

export class ChecklistApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static getAll(issueId) {
        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${issueId}/checklists`)
    }
}
