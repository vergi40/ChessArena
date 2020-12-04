export class SharedData{
    movesReceived = 0;
    movesSent = 0;
    moveHistory = [];

    getNextMoveInJSON() {
        // 
        let data = moveHistory[movesSent];
        let json = JSON.stringify({
            from: data.start_position, 
            to: data.end_position,
            promotion: data.promotion_result,
            diagnostics: data.diagnostics
        });
        return json;
    }
}
