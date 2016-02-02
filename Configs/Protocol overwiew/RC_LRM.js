{

	protocol: "resources",
	nodes: [{
		node: "node1",
		domains: [
			"lowestDomian",
			"higherDomian",
			"highestDomian"
		],
        data: [
	        {	
				port: 1,
				destination: {
					node: "node2",
					port: 2
				},
				status: "FREE"
			},
		    {	
				port: 2,
				destination: {
					node: "node3",
					port: 3
				},
				status: "OCCUPIED"
			}
		]
	},
	{
		node: "node2",
		domains: [
			"highestDomian"
		],
        data: [
	        {	
				port: 2,
				destination: {
					node: "node1",
					port: 1
				},
				status: "FREE"
			},
		    {	
				port: 1,
				destination: {
					node: "node2",
					port: 3
				},
				status: "OCCUPIED"
			}
		]
	}]
}