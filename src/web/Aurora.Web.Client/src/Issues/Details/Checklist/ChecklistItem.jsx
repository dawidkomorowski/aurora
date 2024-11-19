import styles from "./ChecklistItem.module.css"

export function ChecklistItem({ id, content, checked }) {

    function handleInput() {
    }

    return (
        <div className={styles.checklistItem} style={{ display: "flex", margin: "5px 0px 5px 0px" }}>
            <div>
                <input type="checkbox" checked={checked} onChange={handleInput} />
            </div>
            <div>{content}</div>
            <div style={{ marginLeft: "auto" }}>
                <button>Edit</button>
                <button style={{ marginLeft: "5px" }}>Remove</button>
            </div>
        </div>
    );
}
