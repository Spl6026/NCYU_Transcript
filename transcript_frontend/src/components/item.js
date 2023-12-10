const Item = ({Id, Name}) => {
    return <option className="option" value={Id}>
        {Name}
    </option>
}

export default Item