import React from "react";

const Board = (pieces) => {

	// Tachyon explanations: http://tachyons.io/docs/table-of-styles/
	// tc 				= text-align:center
	// bg-light-green 	= background light green
	// dib 				= inline-block -> sets blocks in grid
	// br3 				= border-radius:.5rem
	// pa2 				= adding:var(--spacing-small)
	// ma2 				= margin:var(--spacing-small)
	// grow 			= transform bigger
	// bw2 				= border-width:.25rem
	// shadow-5 		= box-shadow:4px 4px 8px 0px rgba( 0, 0, 0, 0.2 )
	return (
		<div>
			<img alt="board" src={`https://i.stack.imgur.com/at7rZ.gif`} />
		</div>
	);
}

export default Board;