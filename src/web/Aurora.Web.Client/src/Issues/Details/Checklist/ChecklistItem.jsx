import styles from "./ChecklistItem.module.css"

export function ChecklistItem({ id, content, isChecked, onRemove }) {

    function handleInput() {
    }

    function handleRemoveButtonClick() {
        onRemove(id);
    }

    return (
        <div className={styles.checklistItem} style={{ display: "flex", margin: "5px 0px 5px 0px" }}>
            <div>
                <input type="checkbox" checked={isChecked} onChange={handleInput} />
            </div>
            <div>{content}</div>
            <div style={{ marginLeft: "auto" }}>
                <button>Edit</button>
                <button onClick={handleRemoveButtonClick} style={{ marginLeft: "5px" }}>Remove</button>
            </div>
        </div>
    );
}
