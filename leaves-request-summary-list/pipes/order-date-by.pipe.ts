import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
 name: "orderDateBy"
})
export class OrderDateByPipe  implements PipeTransform {
    transform(array: any[], sortProp: string, sortOrder : string) : any[] {
        if (!sortProp)
            return array;
        // console.log('pipe transform: ' + JSON.stringify(array));
        // console.log('pipe transform: ' + sortProp + ' + ' + sortOrder);

        let copiedArray = [...array];

        if (sortOrder === 'asc') {
            return copiedArray.sort((param1, param2) => {
                return new Date(param1[sortProp]).getTime() - new Date(param2[sortProp]).getTime();
            })
        }
        else if (sortOrder === 'desc') {
            return copiedArray.sort((param1, param2) => {
                return new Date(param2[sortProp]).getTime() - new Date(param1[sortProp]).getTime();
            })
        }

        return array;
    }
}