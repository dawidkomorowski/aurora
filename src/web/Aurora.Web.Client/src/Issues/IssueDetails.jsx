export function IssueDetails({ title, description, status, version }) {
    return (
        <div>
            <div><h1>{title}</h1></div>
            <div><h4>Description</h4></div>
            <div style={{ whiteSpace: "pre-wrap" }}>{description}</div>
            <br />
            <div><strong>Status: </strong>{status}</div>
            <div><strong>Version: </strong>{version?.name}</div>
        </div>
    );
}