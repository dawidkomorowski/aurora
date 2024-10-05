
export class IssuesServiceClient {
    static #issuesServiceUrl = "http://localhost:5180/api";
    static getAll() {
        return fetch(`${this.#issuesServiceUrl}/issues`).then(response => {
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        });
    }

    static get(id) {
        return fetch(`${this.#issuesServiceUrl}/issues/${id}`).then(response => {
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        });
    }
}
