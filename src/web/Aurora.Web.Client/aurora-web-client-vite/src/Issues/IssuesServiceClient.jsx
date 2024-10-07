
export class IssuesServiceClient {
    static #issuesServiceUrl = "http://localhost:5180/api";
    static getAll(filters) {
        let uri = `${this.#issuesServiceUrl}/issues`;

        if (filters?.status) {
            uri = `${uri}?status=${encodeURIComponent(filters.status)}`;
        }

        return fetch(uri).then(response => {
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

    static create(title, description) {
        const createIssueRequest = {
            title: title,
            description: description
        }

        const requestInit = {
            method: "POST",
            body: JSON.stringify(createIssueRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return fetch(`${this.#issuesServiceUrl}/issues`, requestInit).then(response => {
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        });
    }

    static update(id, title, description, status) {
        const createIssueRequest = {
            title: title,
            description: description,
            status: status
        }

        const requestInit = {
            method: "PUT",
            body: JSON.stringify(createIssueRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return fetch(`${this.#issuesServiceUrl}/issues/${id}`, requestInit).then(response => {
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        });
    }
}
