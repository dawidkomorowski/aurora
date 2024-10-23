import { useEffect, useState } from "react";
import { IssueList } from "./IssueList";
import { IssuesServiceClient } from "./IssuesServiceClient"

export function IssueExplorer() {
    const [statusFilter, setStatusFilter] = useState("");
    const [data, setData] = useState([]);

    useEffect(() => {
        const filters = {
            status: statusFilter || null
        };

        IssuesServiceClient.getAll(filters).then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error)
        });
    }, [statusFilter]);

    function handleStatusFilterChange(event) {
        setStatusFilter(event.target.value);
    }

    return (
        <div>
            <div style={{ display: "flex" }}>
                <div style={{ margin: "10px" }}>
                    <strong>Status</strong>
                </div>
                <select value={statusFilter} onChange={handleStatusFilterChange} style={{ width: "200px" }}>
                    <option value="">All</option>
                    <option value="Open">Open</option>
                    <option value="In Progress">In Progress</option>
                    <option value="Closed">Closed</option>
                </select>
            </div>
            <IssueList data={data} />
        </div>
    );
}

