import Item from "./item";

const datalist = ({htmlId, SelectData, setOption = () => {}}) => {
    function optionChange(e){
        setOption(e.target.value)
    }

    return (<div className="datalist">
        <input list={htmlId} onChange={optionChange} className="datalist_input"/>
        <datalist id={htmlId}>
            {SelectData.map((item) => {
                const {Id, Name} = item;
                return (
                    <Item
                        Id={Id}
                        Name={Name}
                    />
                );
            })
            }
        </datalist>
    </div>)
}

export default datalist