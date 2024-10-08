import { Link, useNavigate } from "react-router-dom";
import styles from "./IssueList.module.css"

export function IssueList({ data }) {
    const items = data.map(i => {
        return <IssueListItem key={i.id} id={i.id} title={i.title} status={i.status} />
    });

    return (
        <div style={{ backgroundColor: "lightgray", padding: "10px" }}>
            <div>
                {items}
            </div>
        </div>
    );
}

function IssueListItem({ id, title, status }) {
    const navigate = useNavigate();

    function handleClick() {
        navigate(`/issue/${id}`);
    }

    return (
        <div className={styles.listItem} style={{ display: "flex", cursor: "pointer", borderBottomStyle: "solid", borderWidth: "1px" }} onClick={handleClick}>
            <div style={{ width: "75px", textAlign: "center" }}>{id}</div>
            <div style={{ width: "80%" }}>
                <strong>{title}</strong>
            </div>
            <div>{status}</div>
        </div>
    );
}