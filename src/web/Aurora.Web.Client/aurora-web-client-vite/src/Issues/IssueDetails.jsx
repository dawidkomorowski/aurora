export function IssueDetails({ title, description, status }) {
    return (
        <div>
            <div><h1>{title}</h1></div>
            <div><h4>Description</h4></div>
            <div style={{ whiteSpace: "pre-wrap" }}>{description}</div>
            <br />
            <div><strong>Status: </strong>{status}</div>
        </div>
    );
}