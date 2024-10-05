export function IssuesList({ data }) {
    const items = data.map(i => {
        return <IssuesListItem key={i.id} id={i.id} title={i.title} status={i.status} />
    });

    return (
        <>
            <div style={{ backgroundColor: "lightgray" }}>
                <div>Issues List</div>
                <div>
                    {items}
                </div>
            </div>
        </>
    );
}

function IssuesListItem({ id, title, status }) {
    return (
        <>
            <div style={{ display: "flex" }}>
                <div style={{ width: "75px", textAlign: "center" }}>{id}</div>
                <div style={{ width: "80%" }}>{title}</div>
                <div>{status}</div>
            </div>
        </>
    );
}

