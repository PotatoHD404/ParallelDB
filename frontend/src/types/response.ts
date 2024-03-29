export type Response = {
    columns: string[],
    rows: any[][],
    syntaxTree: string
    queryTree: string    
    error: string | null
}