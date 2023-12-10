import Item from "./item";
import {useEffect} from "react";
const select = ({SelectData, setOption = () => {}}) => {
    function optionChange(e){
        setOption(e.target.value)
    }

    // eslint-disable-next-line react-hooks/rules-of-hooks
    useEffect(() => {
        const initialSelectedValue = SelectData.length > 0 ? SelectData[0].Id : "";
        setOption(initialSelectedValue);
    }, [SelectData, setOption]);

    return (<div className="select">
        <select onChange={optionChange}>
            {SelectData.map((item) => {
                const {Id, Name} = item;
                return (
                    <Item Id={Id} Name={Name} />
                );
            })
            }
        </select>
    </div>)
}

export default select