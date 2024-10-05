import { Link } from "react-router-dom";

export function IssueList({ data }) {
    const items = data.map(i => {
        return <IssueListItem key={i.id} id={i.id} title={i.title} status={i.status} />
    });

    return (
        <>
            <div style={{ backgroundColor: "lightgray" }}>
                <div>
                    {items}
                </div>
            </div>
        </>
    );
}

function IssueListItem({ id, title, status }) {
    return (
        <>
            <div style={{ display: "flex" }}>
                <div style={{ width: "75px", textAlign: "center" }}><Link to={`/issue/${id}`}>{id}</Link></div>
                <div style={{ width: "80%" }}>{title}</div>
                <div>{status}</div>
            </div>
        </>
    );
}