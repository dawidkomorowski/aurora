import { IssueListItem } from "./IssueListItem";

export function IssueList({ data }) {
    const items = data.map(i => {
        return <IssueListItem key={i.id} id={i.id} title={i.title} status={i.status} version={i.version} />
    });

    return (
        <div style={{ backgroundColor: "lightgray", padding: "10px" }}>
            <div>
                {items}
            </div>
        </div>
    );
}

