import { ApiClient } from "./ApiClient";

export class ChecklistApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static getAll(issueId) {
        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${issueId}/checklists`)
    }

    static createChecklist(issueId, title) {
        const createChecklistRequest = {
            title: title
        };

        const requestInit = {
            method: "POST",
            body: JSON.stringify(createChecklistRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${issueId}/checklists`, requestInit);
    }
}
