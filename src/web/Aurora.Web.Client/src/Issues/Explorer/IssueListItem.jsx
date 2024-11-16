import { Link } from "react-router-dom";
import styles from "./IssueListItem.module.css";

export function IssueListItem({ id, title, status, version }) {
    return (
        <div className={styles.listItem} style={{ display: "flex", borderBottomStyle: "solid", borderWidth: "1px" }}>
            <div style={{ width: "75px", textAlign: "center" }}>{id}</div>
            <div style={{ width: "80%" }}>
                <Link className={styles.issueLink} to={`/issue/${id}`}><strong>{title}</strong></Link>
            </div>
            <div style={{ width: "10%" }}>{status}</div>
            <div>{version?.name}</div>
        </div>
    );
}